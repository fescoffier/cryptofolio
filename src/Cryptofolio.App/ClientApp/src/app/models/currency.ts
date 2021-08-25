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