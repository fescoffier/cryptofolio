import { CollectionViewer, DataSource } from '@angular/cdk/collections';
import { BehaviorSubject, Observable, Subscription } from 'rxjs';

import { Transaction } from "../../../models/transaction";
import { TransactionService } from "../transaction.service";

export class TransactionsDataSource extends DataSource<Transaction | undefined> {
  private skip = 0;
  private take = 10;
  private cachedTransactions = Array.from<Transaction>({ length: 0 });
  private dataStream = new BehaviorSubject<(Transaction | undefined)[]>(this.cachedTransactions);
  private subscription = new Subscription();

  constructor(private service: TransactionService) {
    super();
    this.fetchTransactions();
  }

  connect(collectionViewer: CollectionViewer): Observable<(Transaction | undefined)[] | ReadonlyArray<Transaction | undefined>> {
    this.subscription.add(collectionViewer.viewChange.subscribe(range => {
      if (range.end === (this.skip + this.take)) {
        this.skip += this.take;
        this.fetchTransactions();
      }
    }));
    return this.dataStream;
  }

  disconnect(collectionViewer: CollectionViewer): void {
    this.subscription.unsubscribe();
  }

  fetchTransactions(): void {
    this.service.list(this.skip, this.take).subscribe(transactions => {
      if (transactions.length > 0) {
        this.cachedTransactions = this.cachedTransactions.concat(transactions);
        this.dataStream.next(this.cachedTransactions);
      }
    });
  }
}