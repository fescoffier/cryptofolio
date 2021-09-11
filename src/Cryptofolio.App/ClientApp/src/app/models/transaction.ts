import { Asset } from "./asset";
import { Currency } from "./currency";
import { Exchange } from "./exchange";
import { Wallet } from "./wallet";

export abstract class Transaction {
  static readonly BuyOrSellType = "BuyOrSellTransaction";
  static readonly TransferType = "TransferTransaction";

  id: string;
  date: Date;
  wallet: Wallet;
  asset: Asset;
  exchange: Exchange;
  qty: number;
  initialValue: number;
  currentValue: number;
  change: number;
  note: string;
  type_discriminator: string;
}

export class BuyOrSellTransaction extends Transaction {
  type: string;
  currency: Currency;
  price: number;

  constructor(properties?: any) {
    super();
    Object.assign(this, properties);
    this.date = new Date(properties.date);
    this.wallet = new Wallet(properties.wallet);
    this.asset = new Asset(properties.asset);
    this.exchange = new Exchange(properties.exchange);
    this.currency = new Currency(properties.currency);
    this.initialValue = properties["initial_value"];
    this.currentValue = properties["current_value"];
  }
}

export class TransferTransaction extends Transaction {
  source: string;
  destination: string;

  constructor(properties?: any) {
    super();
    Object.assign(this, properties);
    this.wallet = new Wallet(properties.wallet);
    this.asset = new Asset(properties.asset);
    this.exchange = new Exchange(properties.exchange);
    this.initialValue = properties["initial_value"];
    this.currentValue = properties["current_value"];
  }
}
