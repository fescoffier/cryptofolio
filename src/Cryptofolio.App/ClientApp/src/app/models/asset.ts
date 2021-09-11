import { Currency } from "./currency";

export class Asset {
  id: string;
  symbol: string;
  name: string;
  description: string;

  constructor(properties?: any) {
    Object.assign(this, properties);
  }
}

export class AssetTicker {
  asset: Asset;
  vsCurrency: Currency;
  timestamp: Date;
  value: number;

  constructor(properties?: any) {
    Object.assign(this, properties);
    this.asset = new Asset(properties.asset);
    this.vsCurrency = new Currency(properties["vs_currency"]);
    this.timestamp = new Date(properties.timestamp);
  }
}