import { Currency } from "./currency";
import { Holding } from "./holding";

export class Wallet {
  id: string;
  name: string;
  description: string;
  currency: Currency;
  initialValue: number;
  currentValue: number;
  change: number;
  selected: boolean;
  holdings: Holding[];

  constructor(properties?: any) {
    Object.assign(this, properties);
    this.currency = new Currency(properties.currency);
    this.initialValue = properties["initial_value"];
    this.currentValue = properties["current_value"];
    this.holdings = properties["holdings"].map(h => new Holding(h));
  }
}