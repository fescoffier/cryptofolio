import { Component } from "@angular/core";
import swal from "sweetalert2";

import { Transaction } from "../../../models/transaction";
import { TransactionService } from "../transaction.service";
import { TransactionsDataSource as TransactionsHistoryDataSource } from "./transactions-history.datasource";

@Component({
  selector: "app-transactions-history",
  templateUrl: "transactions-history.component.html",
  styleUrls: ["transactions-history.component.scss"]
})
export class TransactionsHistoryComponent {
  public datasource: TransactionsHistoryDataSource;

  constructor(private service: TransactionService) {
    this.datasource = new TransactionsHistoryDataSource(service);
  }

  delete(transaction: Transaction) {
    swal.fire({
      title: "Do you really want to delete this transaction?",
      text: "You won't be able to revert this!",
      icon: "warning",
      showCancelButton: true,
      customClass: {
        cancelButton: "btn btn-danger",
        confirmButton: "btn btn-success mr-1",
      },
      confirmButtonText: "Yes",
      cancelButtonText: "No",
      buttonsStyling: false
    })
    .then(result => {
      if (result.value) {
        this.service.delete(transaction).subscribe(() => this.datasource.remove(transaction));
      }
    });
  }
}
