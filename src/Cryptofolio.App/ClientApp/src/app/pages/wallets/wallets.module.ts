import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';

import { PipeModule } from "../../pipes/pipe.module";
import { WalletsComponent } from "./wallets.component";
import { WalletsRoutes } from "./wallets.routing";
import { WalletsService } from "./wallets.service";

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule.withConfig({ warnOnNgModelWithFormControl: "never" }),
    RouterModule.forChild(WalletsRoutes),
    BsDropdownModule,
    PipeModule
  ],
  declarations: [
    WalletsComponent
  ],
  providers: [
    WalletsService
  ]
})
export class WalletsModule {}
