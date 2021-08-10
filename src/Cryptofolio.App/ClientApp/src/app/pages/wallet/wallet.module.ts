import { NgModule } from "@angular/core";
import { RouterModule } from "@angular/router";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";

import { CommonModule } from "@angular/common";
import { WalletsComponent } from "./wallets/wallets.component";
import { WalletRoutes } from "./wallet.routing";
import { WalletService } from "./wallet.service";

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule.withConfig({ warnOnNgModelWithFormControl: "never" }),
    RouterModule.forChild(WalletRoutes)
  ],
  declarations: [
    WalletsComponent
  ],
  providers: [
    WalletService
  ]
})
export class WalletModule {}
