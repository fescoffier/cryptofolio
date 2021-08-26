import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { ScrollingModule } from '@angular/cdk/scrolling';
import { BsDatepickerModule } from "ngx-bootstrap/datepicker";
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { TimepickerModule } from "ngx-bootstrap/timepicker";

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
    ScrollingModule,
    BsDatepickerModule.forRoot(),
    BsDropdownModule.forRoot(),
    TimepickerModule.forRoot()
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
