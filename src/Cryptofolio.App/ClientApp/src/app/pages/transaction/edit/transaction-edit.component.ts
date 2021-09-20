import { Component, OnInit } from "@angular/core";
import { Location } from "@angular/common";
import { ActivatedRoute, Router } from "@angular/router";
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
import { BuyOrSellTransaction, Transaction } from "../../../models/transaction";
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

  public type: string = "buy";

  public form: FormGroup;
  public formMode = "create";
  public formSubmitted = false;
  get formControls() {
    return this.form?.controls;
  }

  constructor(
    private fb: FormBuilder,
    private location: Location,
    private router: Router,
    private route: ActivatedRoute,
    private service: TransactionService) {
  }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      if (params["id"]) {
        this.service
          .get(params["id"])
          .subscribe(transaction => {
            if (transaction instanceof BuyOrSellTransaction) {
              this.type = transaction.type;
            }
            this.formMode = "edit";
            this.form = this.createForm(transaction);
          });
      } else {
        this.form = this.createForm();
      }
    });
    this.service.getAssets().subscribe(assets => this.assets = assets);
    this.service.getAssetsSources().subscribe(assetsSources => this.assetsSources = assetsSources);
    this.service.getAssetsDestinations().subscribe(assetsDestinations => this.assetsDestinations = assetsDestinations);
    this.service.getCurrencies().subscribe(currencies => this.currencies = currencies);
    this.service.getExchanges().subscribe(exchanges => this.exchanges = exchanges);
    this.service
      .getWallets()
      .subscribe(wallets => {
        this.wallets = wallets;
        this.setDefault();
      });
  }

  private createForm(transaction?: Transaction): FormGroup {
    if (this.type === "buy" || this.type === "sell") {
      const form = this.fb.group({
        id: [transaction?.id],
        transaction_date: [transaction?.date, [Validators.required]],
        transaction_time: [transaction?.date],
        wallet_id: [transaction?.wallet.id, [Validators.required]],
        wallet_name: [transaction?.wallet.name],
        asset_id: [transaction?.asset.id, [Validators.required]],
        asset_name: [transaction?.asset.name],
        exchange_id: [transaction?.exchange.id, [Validators.required]],
        exchange_name: [transaction?.exchange.name],
        currency_id: [transaction?.["currency"]["id"], [Validators.required]],
        currency_name: [transaction?.["currency"]["code"]],
        price: [transaction?.["price"], [Validators.required]],
        qty: [transaction?.qty, [Validators.required, Validators.min(0), Validators.max(Number.MAX_VALUE)]],
        note: [transaction?.note]
      });
      form.controls.transaction_date.valueChanges.subscribe(_ => this.setTicker());
      form.controls.transaction_time.valueChanges.subscribe(_ => this.setTicker());
      return form;
    } else if (this.type === "transfer") {
      return this.fb.group({
        id: [transaction?.id],
        transaction_date: [transaction?.date, [Validators.required]],
        transaction_time: [transaction?.date],
        wallet_id: [transaction?.wallet.id, [Validators.required]],
        wallet_name: [transaction?.wallet.name],
        asset_id: [transaction?.asset.id, [Validators.required]],
        asset_name: [transaction?.asset.name],
        exchange_id: [transaction?.exchange.id],
        exchange_name: [transaction?.exchange.name],
        source: [transaction?.["source"], [Validators.required]],
        destination: [transaction?.["destination"], [Validators.required]],
        qty: [transaction?.qty, [Validators.required, Validators.min(0), Validators.max(Number.MAX_VALUE)]],
        note: [transaction?.note]
      }, {
        validators: [this.validateExchange(this.type)]
      });
    }
  }

  private setDefault() {
    const selected = this.wallets.filter(w => w.selected)[0];
        if (selected && this.formMode === "create") {
          const date = new Date();
          this.formControls["transaction_date"].setValue(date);
          this.formControls["transaction_time"].setValue(date);
          this.formControls["wallet_id"].setValue(selected.id);
          this.formControls["wallet_name"].setValue(selected.name);
          if (this.type === "buy" || this.type === "sell") {
            this.formControls["currency_id"].setValue(selected.currency.id);
            this.formControls["currency_name"].setValue(selected.currency.code);
          }
          this.setTicker();
        }
  }

  private setTicker() {
    if (this.type === "buy" || this.type === "sell") {
      const assetId = this.formControls["asset_id"].value;
      if (assetId) {
        const vsCurrency = this.formControls["currency_name"].value;
        const timestamp = this.formControls.transaction_date.value as Date;
        const time = this.formControls.transaction_time.value as Date;
        if (time) {
          timestamp.setHours(time.getHours());
          timestamp.setMinutes(time.getMinutes());
          timestamp.setSeconds(0);
        }
        this.service
          .getAssetTicker(assetId, vsCurrency, timestamp)
          .subscribe(ticker => this.formControls["price"].setValue(ticker.value));
      }
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
    this.setDefault();
  }

  setWallet(wallet: Wallet) {
    this.formControls.wallet_id.setValue(wallet.id);
    this.formControls.wallet_name.setValue(wallet.name);
  }

  setAsset(asset: Asset) {
    this.formControls.asset_id.setValue(asset.id);
    this.formControls.asset_name.setValue(asset.name);
    this.formControls.price.setValue(null);
    this.setTicker();
  }

  setSource(source: string) {
    this.formControls.source.setValue(source);
  }

  setDestination(destination: string) {
    this.formControls.destination.setValue(destination);
  }

  setExchange(exchange: Exchange) {
    this.formControls.exchange_id.setValue(exchange.id);
    this.formControls.exchange_name.setValue(exchange.name);
  }

  setCurrency(currency: Currency) {
    this.formControls.currency_id.setValue(currency.id);
    this.formControls.currency_name.setValue(currency.code);
    this.formControls.price.setValue(null);
    this.setTicker();
  }

  reset() {
    if (this.formMode === "create") {
      this.form.reset();
      this.formSubmitted = false;
    } else {
      this.location.back();
    }
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
    delete transaction.currency_name;
    delete transaction.transaction_date;
    delete transaction.transaction_time;

    if (this.formMode === "create") {
      this.service
      .create(transaction)
      .subscribe(_ => this.router.navigate(["/transactions/history"]));
    } else if (this.formMode === "edit") {
      this.service
        .update(transaction)
        .subscribe(() => this.router.navigate(["/transactions/history"]));
    }
  }
}
