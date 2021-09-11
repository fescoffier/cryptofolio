import { Asset } from "./asset";
import { Wallet } from "./wallet";

export class Holding {
  id: string;
  wallet: Wallet;
  asset: Asset;
  qty: number;
  initialValue: number;
  currentValue: number;
  change: number;

  constructor(properties?: any) {
    Object.assign(this, properties);
    this.wallet = new Wallet(properties.wallet);
    this.asset = new Asset(properties.asset);
    this.initialValue = properties["initial_value"];
    this.currentValue = properties["current_value"];
  }
}