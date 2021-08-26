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
    this.service.getWallets().subscribe(wallets => this.wallets = wallets);
  }

  private createForm(transaction?: Transaction): FormGroup {
    if (this.type === "buy" || this.type === "sell") {
      return this.fb.group({
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
        currency_name: [transaction?.["currency"]["name"]],
        price: [transaction?.["price"], [Validators.required]],
        qty: [transaction?.qty, [Validators.required, Validators.min(0), Validators.max(Number.MAX_VALUE)]],
        note: [transaction?.note]
      });
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
    this.form.controls.currency_id.setValue(currency.id);
    this.form.controls.exchange_name.setValue(currency.code);
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
