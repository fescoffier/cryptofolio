import { Component } from "@angular/core";
import swal from "sweetalert2";

import { TransactionService } from "../transaction.service";
import { TransactionsDataSource } from "./transactions.datasource";

@Component({
  selector: "app-transactions",
  templateUrl: "transactions.component.html",
  styleUrls: ["transactions.component.scss"]
})
export class TransactionsComponent {
  public datasource: TransactionsDataSource;

  constructor(service: TransactionService) {
    this.datasource = new TransactionsDataSource(service);
  }
}
