import { Routes } from "@angular/router";

import { TransactionEditComponent } from "./edit/transaction-edit.component";
import { TransactionsHistoryComponent } from "./history/transactions-history.component";

export const TransactionRoutes: Routes = [
  {
    path: "history",
    component: TransactionsHistoryComponent
  },
  {
    path: "add",
    component: TransactionEditComponent
  }
];
