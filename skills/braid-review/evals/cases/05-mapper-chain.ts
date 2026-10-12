// Case 05 — review the design of this feature slice (all files added in one PR).

// api/dto/AddressDto.ts
export interface AddressDto {
  street: string;
  city: string;
  postcode: string;
  country: string;
}

// domain/Address.ts
export interface Address {
  street: string;
  city: string;
  postcode: string;
  country: string;
}

// persistence/AddressRecord.ts
export interface AddressRecord {
  street: string;
  city: string;
  postcode: string;
  country: string;
}

// api/mappers/addressDtoToDomain.ts
import { AddressDto } from "../dto/AddressDto";
import { Address } from "../../domain/Address";

export function addressDtoToDomain(dto: AddressDto): Address {
  return {
    street: dto.street,
    city: dto.city,
    postcode: dto.postcode,
    country: dto.country,
  };
}

// persistence/mappers/addressDomainToRecord.ts
import { Address } from "../../domain/Address";
import { AddressRecord } from "../AddressRecord";

export function addressDomainToRecord(address: Address): AddressRecord {
  return {
    street: address.street,
    city: address.city,
    postcode: address.postcode,
    country: address.country,
  };
}

// api/routes/updateAddress.ts
import { addressDtoToDomain } from "../mappers/addressDtoToDomain";
import { addressDomainToRecord } from "../../persistence/mappers/addressDomainToRecord";

export async function updateAddress(userId: string, dto: AddressDto): Promise<void> {
  const address = addressDtoToDomain(dto);
  const record = addressDomainToRecord(address);
  await db.addresses.upsert(userId, record);
}
