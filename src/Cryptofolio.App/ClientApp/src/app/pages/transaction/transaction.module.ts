import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { ScrollingModule } from '@angular/cdk/scrolling';

import { TransactionRoutes } from "./transaction.routing";
import { TransactionService } from "./transaction.service";
import { TransactionEditComponent } from "./edit/transaction-edit.component";
import { TransactionsHistoryComponent } from "./history/transactions-history.component";

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule.withConfig({ warnOnNgModelWithFormControl: "never" }),
    RouterModule.forChild(TransactionRoutes),
    ScrollingModule
  ],
  declarations: [
    TransactionsHistoryComponent,
    TransactionEditComponent
  ],
  providers: [
    TransactionService
  ]
})
export class TransactionModule {}
