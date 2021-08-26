export class Exchange {
  id: string;
  name: string;
  description: string;
  year_established: number;
  url: string;
  image: string;

  constructor(properties?: any) {
    Object.assign(this, properties);
  }
}