import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { map } from "rxjs/internal/operators";

import { ApiOptions } from "../../api-options";
import { BuyOrSellTransaction, Transaction, TransferTransaction } from "../../models/transaction";

@Injectable()
export class TransactionService {
  constructor(private http: HttpClient, private api: ApiOptions) {}

  get(): Observable<Transaction[]> {
    return this.http
      .get<any[]>(this.api.transactionsEndpoint)
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