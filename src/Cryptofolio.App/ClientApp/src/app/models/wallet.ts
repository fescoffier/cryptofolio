export class Wallet {
  id: string;
  name: string;
  description: string;
  selected: boolean;

  constructor(properties?: any) {
    Object.assign(this, properties);
  }
}