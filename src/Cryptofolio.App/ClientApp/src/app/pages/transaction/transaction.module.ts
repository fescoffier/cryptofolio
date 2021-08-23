import { NgModule } from "@angular/core";
import { RouterModule } from "@angular/router";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { ScrollingModule } from '@angular/cdk/scrolling';

import { CommonModule } from "@angular/common";
import { TransactionsComponent } from "./transactions/transactions.component";
import { TransactionRoutes } from "./transaction.routing";
import { TransactionService } from "./transaction.service";

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule.withConfig({ warnOnNgModelWithFormControl: "never" }),
    RouterModule.forChild(TransactionRoutes),
    ScrollingModule
  ],
  declarations: [
    TransactionsComponent
  ],
  providers: [
    TransactionService
  ]
})
export class TransactionModule {}
