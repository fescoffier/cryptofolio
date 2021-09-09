export class Currency {
  id: string;
  name: string;
  code: string;
  symbol: string;
  precision: number;

  constructor(properties?: any) {
    Object.assign(this, properties);
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