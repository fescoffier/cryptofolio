import { Component } from "@angular/core";
import swal from "sweetalert2";

import { TransactionService } from "../transaction.service";
import { TransactionsDataSource as TransactionsHistoryDataSource } from "./transactions-history.datasource";

@Component({
  selector: "app-transactions-history",
  templateUrl: "transactions-history.component.html",
  styleUrls: ["transactions-history.component.scss"]
})
export class TransactionsHistoryComponent {
  public datasource: TransactionsHistoryDataSource;

  constructor(service: TransactionService) {
    this.datasource = new TransactionsHistoryDataSource(service);
  }
}
