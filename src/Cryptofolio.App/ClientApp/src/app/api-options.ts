import { Injectable } from "@angular/core";

@Injectable()
export class ApiOptions {
  private _url: string;
  public get url() {
    return this._url;
  }

  private _walletsEndpoint: string;
  public get walletsEndpoint() {
    return this._walletsEndpoint;
  }

  private _transactionsEndpoint: string;
  public get transactionsEndpoint() {
    return this._transactionsEndpoint;
  }

  constructor() {
    const apiOptions = (window as any)['__me']['api'];
    this._url = apiOptions.url;
    this._walletsEndpoint = apiOptions.walletsEndpoint;
    this._transactionsEndpoint = apiOptions.transactionsEndpoint;
  }
}