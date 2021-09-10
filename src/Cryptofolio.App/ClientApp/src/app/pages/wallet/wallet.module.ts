import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';

import { PipeModule } from "../../pipes/pipe.module";
import { WalletsComponent } from "./wallets/wallets.component";
import { WalletRoutes } from "./wallet.routing";
import { WalletService } from "./wallet.service";

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule.withConfig({ warnOnNgModelWithFormControl: "never" }),
    RouterModule.forChild(WalletRoutes),
    BsDropdownModule,
    PipeModule
  ],
  declarations: [
    WalletsComponent
  ],
  providers: [
    WalletService
  ]
})
export class WalletModule {}
