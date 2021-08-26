import { Component, Input, OnInit } from "@angular/core";
import { Router } from "@angular/router";
import {
  FormGroup,
  FormBuilder,
  Validators,
  AbstractControl,
  ValidatorFn,
  ValidationErrors
} from "@angular/forms";

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
  public assetsSources: string[];
  public assetsDestinations: string[];
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

  constructor(private fb: FormBuilder, private router: Router, private service: TransactionService) {
    this.form = this.createForm();
  }

  ngOnInit(): void {
    this.service.getAssets().subscribe(assets => this.assets = assets);
    this.service.getAssetsSources().subscribe(assetsSources => this.assetsSources = assetsSources);
    this.service.getAssetsDestinations().subscribe(assetsDestinations => this.assetsDestinations = assetsDestinations);
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
        currency: [this.transaction?.["currency"], [Validators.required]],
        price: [this.transaction?.["price"], [Validators.required]],
        qty: [this.transaction?.qty, [Validators.required, Validators.min(0), Validators.max(Number.MAX_VALUE)]],
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
        exchange_id: [this.transaction?.exchange.id],
        exchange_name: [this.transaction?.exchange.name],
        source: [this.transaction?.["source"], [Validators.required]],
        destination: [this.transaction?.["destination"], [Validators.required]],
        qty: [this.transaction?.qty, [Validators.required, Validators.min(0), Validators.max(Number.MAX_VALUE)]],
        note: [this.transaction?.note]
      }, {
        validators: [this.validateExchange(this.type)]
      });
    }
  }

  private validateExchange(type: string): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (type === "transfer") {
        const source = control.get("source");
        const destination = control.get("destination");
        if (source.value && destination.value && source.value === destination.value) {
          return {
            source: true,
            destination: true
          }
        }
        const exchangeId = control.get("exchange_id");
        return (source.value === "My exchange" || destination.value === "My exchange") && !exchangeId.value
          ? { exchange_id: true }
          : null;
      }
      return null;
    };
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

  setSource(source: string) {
    this.form.controls.source.setValue(source);
  }

  setDestination(destination: string) {
    this.form.controls.destination.setValue(destination);
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

    const transaction = {
      type: this.type,
      ...this.form.value
    };
    const date = this.form.value.transaction_date as Date;
    const time = this.form.value.transaction_time as Date;
    if (time) {
      date.setHours(time.getHours());
      date.setMinutes(time.getMinutes());
      date.setSeconds(0);
    }
    transaction["date"] = date;

    delete transaction.wallet_name;
    delete transaction.asset_name;
    delete transaction.exchange_name;
    delete transaction.transaction_date;
    delete transaction.transaction_time;

    this.service
      .create(transaction)
      .subscribe(_ => this.router.navigate(["/transactions/history"]));
  }
}
