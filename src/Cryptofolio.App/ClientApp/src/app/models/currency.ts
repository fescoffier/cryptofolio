export class Currency {
  id: string;
  name: string;
  code: string;
  symbol: string;
  precision: number;
  valueFormat: string;

  constructor(properties?: any) {
    Object.assign(this, properties);
    this.valueFormat = properties["value_format"];
  }
}

export class CurrencyTicker {
  currency: Currency;
  vsCurrency: Currency;
  timestamp: Date;
  value: number;

  constructor(properties?: any) {
    Object.assign(this, properties);
    this.currency = new Currency(properties.currency);
    this.vsCurrency = new Currency(properties["vs_currency"]);
    this.timestamp = new Date(properties.timestamp);
  }
}