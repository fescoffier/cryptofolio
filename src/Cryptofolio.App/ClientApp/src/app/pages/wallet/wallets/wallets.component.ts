import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { Currency } from "src/app/models/currency";
import swal from "sweetalert2";

import { Wallet } from "../../../models/wallet";
import { WalletService } from "../wallet.service";

@Component({
  selector: "app-wallets",
  templateUrl: "wallets.component.html"
})
export class WalletsComponent implements OnInit {
  public wallets: Wallet[];
  public currencies: Currency[];

  public form: FormGroup;
  public formSubmitted = false;
  public formDisplayed = false;
  private formMode = "";
  get formControls() {
    return this.form.controls;
  }

  constructor(private formBuilder: FormBuilder, private service: WalletService) { }

  ngOnInit() {
    this.service.get().subscribe(wallets => this.wallets = wallets);
    this.service.getCurrencies().subscribe(currencies => this.currencies = currencies);
    this.form = this.formBuilder.group(
      {
        id: [null],
        name: [null, [Validators.required, Validators.maxLength(250)]],
        currency_id: [null, [Validators.required]],
        currency_name: [null],
        description: [null]
      }
    );
  }

  onFormSubmit() {
    this.formSubmitted = true;

    if (this.form.invalid) {
      return;
    }

    if (this.formMode === "create") {
      this.service
        .create(this.form.value)
        .subscribe(wallet => {
          this.wallets.push(wallet);
          this.resetForm();
          swal.fire({
            title: `Would you like to define your newly created wallet as your selected wallet?`,
            text: "You can change your selected wallet anytime.",
            icon: "info",
            showCancelButton: true,
            customClass: {
              cancelButton: "btn btn-danger",
              confirmButton: "btn btn-success mr-1",
            },
            confirmButtonText: "Yes",
            cancelButtonText: "No",
            buttonsStyling: false
          })
          .then(result => {
            if (result.value) {
              this.select(wallet);
            }
          });
        });
    } else if (this.formMode === "edit") {
      this.service
        .update(this.form.value)
        .subscribe(() => {
          const wallet = this.form.value as Wallet;
          const index = this.wallets.findIndex(w => w.id == wallet.id);
          this.wallets[index] = wallet;
          this.resetForm();
        });
    }
  }

  setCurrency(currency: Currency) {
    this.form.controls.currency_id.setValue(currency.id);
    this.form.controls.currency_name.setValue(currency.code);
  }

  create() {
    this.form.reset();
    this.formSubmitted = false;
    this.formDisplayed = true;
    this.formMode = "create";
  }

  edit(wallet: Wallet) {
    this.form.controls["id"].setValue(wallet.id);
    this.form.controls["name"].setValue(wallet.name);
    this.form.controls["currency_id"].setValue(wallet.currency.id);
    this.form.controls["currency_name"].setValue(wallet.currency.code);
    this.form.controls["description"].setValue(wallet.description);
    this.formSubmitted = false;
    this.formDisplayed = true;
    this.formMode = "edit";
  }

  cancelEdit() {
    this.resetForm();
  }

  private resetForm() {
    this.formDisplayed = false;
    this.formSubmitted = false;
    this.formMode = "";
    this.form.reset();
  }

  select(wallet: Wallet) {
    this.service
      .select(wallet)
      .subscribe(() => {
        this.wallets.forEach(w => w.selected = false);
        wallet.selected = true;
      });
  }

  delete(wallet: Wallet) {
    if (this.wallets.length == 1) {
      swal.fire({
        title: "Sorry! You can't delete your last wallet.",
        buttonsStyling: false,
        customClass: {
          confirmButton: "btn btn-success",
        },
        icon: "warning"
      });
    } else if (wallet.selected) {
      swal.fire({
        title: "Sorry! You can't delete your selected wallet.",
        buttonsStyling: false,
        customClass: {
          confirmButton: "btn btn-success",
        },
        icon: "warning"
      });
    } else {
      swal.fire({
        title: `Do you really want to delete the wallet \"${wallet.name}\" and its associated transactions?`,
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        customClass: {
          cancelButton: "btn btn-danger",
          confirmButton: "btn btn-success mr-1",
        },
        confirmButtonText: "Yes",
        cancelButtonText: "No",
        buttonsStyling: false
      })
      .then(result => {
        if (result.value) {
          this.service.delete(wallet).subscribe(() => this.wallets.splice(this.wallets.indexOf(wallet), 1));
        }
      });
    }
  }
}
