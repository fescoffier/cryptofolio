import { Inject, Injectable, LOCALE_ID } from "@angular/core";
import { formatDate } from "@angular/common";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { map } from "rxjs/internal/operators";

import { ApiOptions } from "../../api-options";
import { Asset, AssetTicker } from "../../models/asset";
import { Currency } from "../../models/currency";
import { Exchange } from "../../models/exchange";
import { BuyOrSellTransaction, Transaction, TransferTransaction } from "../../models/transaction";
import { Wallet } from "../../models/wallet";

@Injectable()
export class TransactionService {
  constructor(@Inject(LOCALE_ID) private locale: string, private http: HttpClient, private api: ApiOptions) {}

  getWallets(): Observable<Wallet[]> {
    return this.http
      .get<any[]>(this.api.walletsEndpoint)
      .pipe(
        map(wallets => wallets.map(w => new Wallet(w)))
      );
  }

  getAssetTicker(assetId: string, vsCurrency: string, timestamp: Date): Observable<AssetTicker> {
    return this.http
      .get<any>(`${this.api.assetsEndpoint}/${assetId}/tickers/${vsCurrency}/${(timestamp ? formatDate(timestamp, "yyyy-MM-ddTHH:mmZ", this.locale) : "latest")}`)
      .pipe(
        map(ticker => new AssetTicker(ticker))
      );
  }

  getAssets(): Observable<Asset[]> {
    return this.http
      .get<any[]>(this.api.assetsEndpoint)
      .pipe(
        map(assets => assets.map(a => new Asset(a)))
      );
  }

  getAssetsSources(): Observable<string[]> {
    return this.http.get<string[]>(`${this.api.assetsEndpoint}/sources`);
  }

  getAssetsDestinations(): Observable<string[]> {
    return this.http.get<string[]>(`${this.api.assetsEndpoint}/destinations`);
  }

  getExchanges(): Observable<Exchange[]> {
    return this.http
      .get<any[]>(this.api.exchangesEndpoint)
      .pipe(
        map(exhanges => exhanges.map(e => new Exchange(e)))
      );
  }

  getCurrencies(): Observable<Currency[]> {
    return this.http
      .get<any[]>(this.api.currenciesEndpoint)
      .pipe(
        map(currencies => currencies.map(e => new Currency(e)))
      );
  }

  get(id: string): Observable<Transaction> {
    return this.http
      .get<any>(`${this.api.transactionsEndpoint}/${id}`)
      .pipe(
        map(transaction => this.deserialize(transaction))
      );
  }

  list(skip: number, take: number): Observable<Transaction[]> {
    return this.http
      .get<any[]>(`${this.api.transactionsEndpoint}?skip=${skip}&take=${take}`)
      .pipe(
        map(transactions => transactions.map(t => this.deserialize(t)))
      );
  }

  create(transaction: Transaction): Observable<Transaction> {
    return this.http
      .post<any>(this.api.transactionsEndpoint, transaction)
      .pipe(
        map(transaction => this.deserialize(transaction))
      );
  }

  update(transaction: Transaction): Observable<void> {
    return this.http.put<void>(this.api.transactionsEndpoint, transaction);
  }

  delete(transaction: Transaction): Observable<void> {
    return this.http.delete<void>(`${this.api.transactionsEndpoint}/${transaction.id}`);
  }

  private deserialize(item: { type_discriminator: string; }): Transaction {
    if (item.type_discriminator === Transaction.BuyOrSellType) {
      return new BuyOrSellTransaction(item);
    } else if (item.type_discriminator === Transaction.TransferType) {
      return new TransferTransaction(item);
    }
  }
}