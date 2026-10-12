// Case 04 — review the design of this import pipeline (three "stages" packages).

package pipeline

// stages/context.go
type ImportContext struct {
	RawRows      [][]string
	ParsedOrders []Order
	Warnings     []string
	CustomerIDs  map[string]int64
	Validated    bool
}

// stages/parse.go
type ParseStage struct{}

func (ParseStage) Run(ctx *ImportContext) error {
	for _, row := range ctx.RawRows {
		order, warn, err := parseRow(row)
		if err != nil {
			return err
		}
		if warn != "" {
			ctx.Warnings = append(ctx.Warnings, warn)
		}
		ctx.ParsedOrders = append(ctx.ParsedOrders, order)
	}
	return nil
}

// stages/resolve.go
type ResolveStage struct{ DB *sql.DB }

// Run requires ParseStage to have populated ParsedOrders first.
func (s ResolveStage) Run(ctx *ImportContext) error {
	for i, order := range ctx.ParsedOrders {
		id, err := lookupCustomer(s.DB, order.CustomerEmail)
		if err != nil {
			return err
		}
		ctx.CustomerIDs[order.CustomerEmail] = id
		ctx.ParsedOrders[i].CustomerID = id
	}
	return nil
}

// stages/validate.go
type ValidateStage struct{}

// Run must come after ResolveStage: validation checks CustomerID != 0.
func (ValidateStage) Run(ctx *ImportContext) error {
	for _, order := range ctx.ParsedOrders {
		if order.CustomerID == 0 {
			return fmt.Errorf("order %s: unresolved customer", order.Ref)
		}
	}
	ctx.Validated = true
	return nil
}

// importer.go
func RunImport(rows [][]string, db *sql.DB) (*ImportContext, error) {
	ctx := &ImportContext{RawRows: rows, CustomerIDs: map[string]int64{}}
	stages := []interface{ Run(*ImportContext) error }{
		ParseStage{}, ResolveStage{DB: db}, ValidateStage{},
	}
	for _, stage := range stages {
		if err := stage.Run(ctx); err != nil {
			return nil, err
		}
	}
	return ctx, nil
}
