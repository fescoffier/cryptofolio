import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";

import { WalletModule } from "./wallet.module";
import { Wallet } from "src/app/models/wallet";
import { Observable } from "rxjs";

@Injectable({ providedIn: WalletModule })
export class WalletService {
  constructor(private http: HttpClient) {}

  get(): Observable<Wallet[]> {
    return this.http.get<Wallet[]>("https://localhost:5100/wallets", { withCredentials: true });
  }

  create(wallet: Wallet): Observable<Wallet> {
    return this.http.post<Wallet>("https://localhost:5100/wallets", wallet, { withCredentials: true });
  }

  update(wallet: Wallet): Observable<void> {
    return this.http.put<void>("https://localhost:5100/wallets", wallet, { withCredentials: true });
  }

  delete(wallet: Wallet): Observable<void> {
    return this.http.delete<void>(`https://localhost:5100/wallets/${wallet.id}`, { withCredentials: true });
  }
}