import { Currency } from "./currency";

export class Asset {
  id: string;
  symbol: string;
  name: string;
  description: string;
  thumbImageUrl: string;
  smallImageUrl: string;
  largeImageUrl: string;

  constructor(properties?: any) {
    Object.assign(this, properties);
    this.thumbImageUrl = properties["thumb_image_url"];
    this.smallImageUrl = properties["small_image_url"];
    this.largeImageUrl = properties["large_image_url"];
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