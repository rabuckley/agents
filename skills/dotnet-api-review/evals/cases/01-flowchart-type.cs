// Case 01 — review the public API of this type.

namespace Contoso.Import;

public class CsvImporter
{
    private string? _path;
    private List<string[]>? _rows;
    private bool _validated;

    public IReadOnlyList<string[]> Rows => _rows ?? throw new InvalidOperationException("Call Parse first.");
    public IReadOnlyList<ImportError> Errors { get; } = new List<ImportError>();

    public void Load(string path)
    {
        _path = path ?? throw new ArgumentNullException(nameof(path));
    }

    public void Parse()
    {
        if (_path is null) throw new InvalidOperationException("Call Load first.");
        _rows = File.ReadAllLines(_path).Select(line => line.Split(',')).ToList();
    }

    public void Validate(CsvSchema schema)
    {
        if (_rows is null) throw new InvalidOperationException("Call Parse first.");
        foreach (var (row, index) in _rows.Select((r, i) => (r, i)))
            if (row.Length != schema.ColumnCount)
                ((List<ImportError>)Errors).Add(new ImportError(index, "Wrong column count"));
        _validated = true;
    }

    public ImportResult Commit(IImportTarget target)
    {
        if (!_validated) throw new InvalidOperationException("Call Validate first.");
        if (Errors.Count > 0) throw new InvalidOperationException("Cannot commit with errors.");
        target.Write(_rows!);
        return new ImportResult(_rows!.Count);
    }
}

public sealed record ImportError(int Row, string Message);
public sealed record ImportResult(int RowsWritten);
public sealed record CsvSchema(int ColumnCount);
public interface IImportTarget { void Write(IReadOnlyList<string[]> rows); }
