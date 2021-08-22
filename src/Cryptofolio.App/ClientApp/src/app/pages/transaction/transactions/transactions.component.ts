import { Component, OnInit } from "@angular/core";
import swal from "sweetalert2";

import { Transaction } from "../../../models/transaction";
import { TransactionService } from "../transaction.service";

@Component({
  selector: "app-transactions",
  templateUrl: "transactions.component.html"
})
export class TransactionsComponent implements OnInit {
  public transactions: Transaction[];

  constructor(private service: TransactionService) { }

  ngOnInit() {
    this.service.get().subscribe(transactions => {
      this.transactions = transactions;
      console.log(this.transactions);
    });
  }
}
