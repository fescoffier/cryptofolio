import { Component, Input } from "@angular/core";
import { FormGroup, FormBuilder, Validators } from "@angular/forms";

import { BuyOrSellTransaction, Transaction, TransferTransaction } from "../../../models/transaction";
import { TransactionService } from "../transaction.service";

@Component({
  selector: "app-transaction-edit",
  templateUrl: "transaction-edit.component.html"
})
export class TransactionEditComponent {

  @Input()
  public transaction: Transaction;

  @Input()
  public type: string = "buy";

  public form: FormGroup;
  public formMode = "create";
  public formSubmitted = false;
  get formControls() {
    return this.form.controls;
  }

  constructor(private fb: FormBuilder, private service: TransactionService) {
    this.form = this.createForm();
  }

  private createForm(): FormGroup {
    if (this.type === "buy" || this.type === "sell") {
      return this.fb.group({
        id: [this.transaction?.id],
        transaction_date: [this.transaction?.date, [Validators.required]],
        transaction_time: [this.transaction?.date],
        wallet_id: [this.transaction?.wallet.id, [Validators.required]],
        asset_id: [this.transaction?.asset.id, [Validators.required]],
        exchange_id: [this.transaction?.exchange.id, [Validators.required]],
        type: [this.transaction?.["type"] || this.type],
        currency: [this.transaction?.["currency"], [Validators.required]],
        price: [this.transaction?.["price"], [Validators.required]],
        qty: [this.transaction?.qty, [Validators.min(0), Validators.max(Number.MAX_VALUE)]],
        note: [this.transaction?.note]
      });
    } else if (this.type === "transfer") {
      return this.fb.group({
        id: [this.transaction?.id],
        transaction_date: [this.transaction?.date, [Validators.required]],
        transaction_time: [this.transaction?.date],
        wallet_id: [this.transaction?.wallet.id, [Validators.required]],
        asset_id: [this.transaction?.asset.id, [Validators.required]],
        exchange_id: [this.transaction?.exchange.id, [Validators.required]],
        source: [this.transaction?.["source"], [Validators.required]],
        destination: [this.transaction?.["destination"], [Validators.required]],
        qty: [this.transaction?.qty, [Validators.min(0), Validators.max(Number.MAX_VALUE)]],
        note: [this.transaction?.note]
      });
    }
  }

  setType(type: string) {
    this.type = type;
    this.form = this.createForm();
    this.formSubmitted = false;
  }

  reset() {
    this.form.reset();
    this.formSubmitted = false;
  }

  submit() {
    this.formSubmitted = true;
    if (!this.form.valid) {
      return;
    }

    const transaction = { ...this.form.value };
    const date = this.form.value.transaction_date as Date;
    const time = this.form.value.transaction_time as Date;
    if (time) {
      date.setTime(time.getTime());
    }
    transaction["date"] = date;
    delete transaction.transaction_date;
    delete transaction.transaction_time;
    console.log(transaction);
  }
}
