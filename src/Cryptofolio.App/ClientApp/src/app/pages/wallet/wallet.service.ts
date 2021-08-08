import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";

import { ApiOptions } from "../../api-options";
import { Wallet } from "../../models/wallet";

@Injectable()
export class WalletService {
  constructor(private http: HttpClient, private api: ApiOptions) {}

  get(): Observable<Wallet[]> {
    return this.http.get<Wallet[]>(this.api.walletsEndpoint);
  }

  create(wallet: Wallet): Observable<Wallet> {
    return this.http.post<Wallet>(this.api.walletsEndpoint, wallet);
  }

  update(wallet: Wallet): Observable<void> {
    return this.http.put<void>(this.api.walletsEndpoint, wallet);
  }

  delete(wallet: Wallet): Observable<void> {
    return this.http.delete<void>(`${this.api.walletsEndpoint}/${wallet.id}`);
  }
}