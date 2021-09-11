import { Currency } from "./currency";

export class Wallet {
  id: string;
  name: string;
  description: string;
  currency: Currency;
  initialValue: number;
  currentValue: number;
  change: number;
  selected: boolean;

  constructor(properties?: any) {
    Object.assign(this, properties);
    this.currency = new Currency(properties.currency);
    this.initialValue = properties["initial_value"];
    this.currentValue = properties["current_value"];
  }
}