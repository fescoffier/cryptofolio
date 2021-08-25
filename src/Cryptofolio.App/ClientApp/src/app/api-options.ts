import { Injectable } from "@angular/core";

@Injectable()
export class ApiOptions {
  private _url: string;
  public get url() {
    return this._url;
  }

  private _assetsEndpoint: string;
  public get assetsEndpoint() {
    return this._assetsEndpoint;
  }

  private _currenciesEndpoint: string;
  public get currenciesEndpoint() {
    return this._currenciesEndpoint;
  }

  private _exchangesEndpoint: string;
  public get exchangesEndpoint() {
    return this._exchangesEndpoint;
  }

  private _transactionsEndpoint: string;
  public get transactionsEndpoint() {
    return this._transactionsEndpoint;
  }
  
  private _walletsEndpoint: string;
  public get walletsEndpoint() {
    return this._walletsEndpoint;
  }

  constructor() {
    const apiOptions = (window as any)['__me']['api'];
    this._url = apiOptions.url;
    this._assetsEndpoint = apiOptions.assetsEndpoint;
    this._currenciesEndpoint = apiOptions.currenciesEndpoint;
    this._exchangesEndpoint = apiOptions.exchangesEndpoint;
    this._transactionsEndpoint = apiOptions.transactionsEndpoint;
    this._walletsEndpoint = apiOptions.walletsEndpoint;
  }
}