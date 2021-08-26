export class Asset {
  id: string;
  symbol: string;
  name: string;
  description: string;

  constructor(properties?: any) {
    Object.assign(this, properties);
  }
}