# az-retail-prices

## Data properties

### Optional properties

Can be missing from the JSON:

- `reservationTerm`
- `effectiveEndDate`

Can be empty strings:

- `armRegionName`
- `location`
- `armSkuName`

### Natural keys

The combination of the following properties is a natural key (unique) for the Azure Retail Prices items.

- `meterId`
- `meterName`
- `priceType`
- `skuId`
- `tierMinimumUnits`

### Relationships between properties

These are just the string properties.

Implies:
- `meterId` ⇒ `unitOfMeasure`
- `productId` ⇒ `serviceName`
- `productId` ⇒ `serviceId`
- `productId` ⇒ `serviceFamily`
- `productName` ⇒ `serviceName`
- `productName` ⇒ `serviceId`
- `productName` ⇒ `serviceFamily`
- `serviceId` ⇒ `serviceFamily`
- `serviceName` ⇒ `serviceFamily`
- `skuId` ⇒ `armRegionName`
- `skuId` ⇒ `location`
- `skuId` ⇒ `productId`
- `skuId` ⇒ `productName`
- `skuId` ⇒ `skuName`
- `skuId` ⇒ `serviceName`
- `skuId` ⇒ `serviceId`
- `skuId` ⇒ `serviceFamily`
- `skuId` ⇒ `armSkuName`
- `skuId` ⇒ `reservationTerm`

Note that `currencyCode` property is a constant value based on the `currencyCode` query parameter. It was excluded from
this list since it's not interesting.

Equivalencies:
- `armRegionName` ⇔ `location`
- `productId` ⇔ `productName`
- `serviceId` ⇔ `serviceName`

### Diagram

```mermaid
erDiagram

  ArmSkuNames {
    INTEGER Id
    TEXT Value
  }

  MeterNames {
    INTEGER Id
    TEXT Value
  }

  PriceTypes {
    INTEGER Id
    TEXT Value
  }

  Regions {
    INTEGER Id
    TEXT ArmRegionName
    TEXT Location
  }

  ReservationTerms {
    INTEGER Id
    TEXT Value
  }

  ServiceFamilies {
    INTEGER Id
    TEXT Value
  }

  SkuNames {
    INTEGER Id
    TEXT Value
  }

  UnitOfMeasures {
    INTEGER Id
    TEXT Value
  }

  Meters {
    INTEGER Id
    TEXT MeterId
    INTEGER UnitOfMeasureId
  }

  Services {
    INTEGER Id
    TEXT ServiceId
    TEXT ServiceName
    INTEGER ServiceFamilyId
  }

  Products {
    INTEGER Id
    TEXT ProductId
    TEXT ProductName
    INTEGER ServiceId
  }

  Skus {
    INTEGER Id
    INTEGER SkuNameId
    INTEGER ArmSkuNameId
    INTEGER RegionId
    INTEGER ProductId
    TEXT SkuIdSuffix
    INTEGER ReservationTermId
  }

  Prices {
    INTEGER Id
    INTEGER MeterId
    INTEGER MeterNameId
    INTEGER PriceTypeId
    INTEGER SkuId
    TEXT TierMinimumUnits
    TEXT RetailPrice
    TEXT UnitPrice
    INTEGER EffectiveStartDate
    INTEGER IsPrimaryMeterRegion
    INTEGER EffectiveEndDate
  }

  ArmSkuNames ||--o{ Skus : "foreign key"

  MeterNames ||--o{ Prices : "foreign key"

  PriceTypes ||--o{ Prices : "foreign key"

  Regions ||--o{ Skus : "foreign key"

  ReservationTerms ||--o{ Skus : "foreign key"

  ServiceFamilies ||--o{ Services : "foreign key"

  SkuNames ||--o{ Skus : "foreign key"

  UnitOfMeasures ||--o{ Meters : "foreign key"

  Meters ||--o{ Prices : "foreign key"

  Services ||--o{ Products : "foreign key"

  Products ||--o{ Skus : "foreign key"

  Skus ||--o{ Prices : "foreign key"
```