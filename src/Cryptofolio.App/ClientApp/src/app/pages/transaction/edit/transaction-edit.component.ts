import { Component, Input, OnInit } from "@angular/core";
import { FormGroup, FormBuilder, Validators } from "@angular/forms";

import { Asset } from "../../../models/asset";
import { Currency } from "../../../models/currency";
import { Exchange } from "../../../models/exchange";
import { Transaction } from "../../../models/transaction";
import { TransactionService } from "../transaction.service";
import { Wallet } from "../../../models/wallet";

@Component({
  selector: "app-transaction-edit",
  templateUrl: "transaction-edit.component.html",
  styleUrls: ["./transaction-edit.component.scss"]
})
export class TransactionEditComponent implements OnInit {
  public assets: Asset[];
  public currencies: Currency[];
  public exchanges: Exchange[];
  public wallets: Wallet[];

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

  ngOnInit(): void {
    this.service.getAssets().subscribe(assets => this.assets = assets);
    this.service.getCurrencies().subscribe(currencies => this.currencies = currencies);
    this.service.getExchanges().subscribe(exchanges => this.exchanges = exchanges);
    this.service.getWallets().subscribe(wallets => this.wallets = wallets);
  }

  private createForm(): FormGroup {
    if (this.type === "buy" || this.type === "sell") {
      return this.fb.group({
        id: [this.transaction?.id],
        transaction_date: [this.transaction?.date, [Validators.required]],
        transaction_time: [this.transaction?.date],
        wallet_id: [this.transaction?.wallet.id, [Validators.required]],
        wallet_name: [this.transaction?.wallet.name],
        asset_id: [this.transaction?.asset.id, [Validators.required]],
        asset_name: [this.transaction?.asset.name],
        exchange_id: [this.transaction?.exchange.id, [Validators.required]],
        exchange_name: [this.transaction?.exchange.name],
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
        wallet_name: [this.transaction?.wallet.name],
        asset_id: [this.transaction?.asset.id, [Validators.required]],
        asset_name: [this.transaction?.asset.name],
        exchange_id: [this.transaction?.exchange.id, [Validators.required]],
        exchange_name: [this.transaction?.exchange.name],
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

  setWallet(wallet: Wallet) {
    this.form.controls.wallet_id.setValue(wallet.id);
    this.form.controls.wallet_name.setValue(wallet.name);
  }

  setAsset(asset: Asset) {
    this.form.controls.asset_id.setValue(asset.id);
    this.form.controls.asset_name.setValue(asset.name);
  }

  setExchange(exchange: Exchange) {
    this.form.controls.exchange_id.setValue(exchange.id);
    this.form.controls.exchange_name.setValue(exchange.name);
  }

  setCurrency(currency: Currency) {
    this.form.controls.currency.setValue(currency.code);
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
    
    delete transaction.wallet_name;
    delete transaction.asset_name;
    delete transaction.exchange_name;
    delete transaction.transaction_date;
    delete transaction.transaction_time;
    console.log(transaction);
  }
}
