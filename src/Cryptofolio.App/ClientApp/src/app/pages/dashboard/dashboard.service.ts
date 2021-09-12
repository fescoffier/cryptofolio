import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { map } from "rxjs/internal/operators";

import { ApiOptions } from "../../api-options";
import { Currency } from "../../models/currency";
import { Wallet } from "../../models/wallet";

@Injectable()
export class DashboardService {
  constructor(private http: HttpClient, private api: ApiOptions) {}

  getWallets(): Observable<Wallet[]> {
    return this.http
      .get<any[]>(this.api.walletsEndpoint)
      .pipe(
        map(wallets => wallets.map(w => new Wallet(w)))
      );
  }
}