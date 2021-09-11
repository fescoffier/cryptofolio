import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { map } from "rxjs/internal/operators";

import { ApiOptions } from "../../api-options";
import { Currency } from "../../models/currency";
import { Wallet } from "../../models/wallet";

@Injectable()
export class WalletService {
  constructor(private http: HttpClient, private api: ApiOptions) {}

  getCurrencies(): Observable<Currency[]> {
    return this.http
      .get<any[]>(this.api.currenciesEndpoint)
      .pipe(
        map(currencies => currencies.map(e => new Currency(e)))
      );
  }

  get(): Observable<Wallet[]> {
    return this.http
      .get<any[]>(this.api.walletsEndpoint)
      .pipe(
        map(wallets => wallets.map(w => new Wallet(w)))
      );
  }

  create(wallet: Wallet): Observable<Wallet> {
    return this.http
      .post<any>(this.api.walletsEndpoint, wallet)
      .pipe(
        map(wallet => new Wallet(wallet))
      );
  }

  update(wallet: Wallet): Observable<void> {
    return this.http.put<void>(this.api.walletsEndpoint, wallet);
  }

  select(wallet: Wallet): Observable<void> {
    return this.http.put<void>(`${this.api.walletsEndpoint}/${wallet.id}/select`, wallet);
  }

  delete(wallet: Wallet): Observable<void> {
    return this.http.delete<void>(`${this.api.walletsEndpoint}/${wallet.id}`);
  }
}