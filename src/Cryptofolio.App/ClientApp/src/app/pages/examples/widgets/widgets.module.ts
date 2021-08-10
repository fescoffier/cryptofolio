import { NgModule } from "@angular/core";
import { RouterModule } from "@angular/router";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { JwBootstrapSwitchNg2Module } from "jw-bootstrap-switch-ng2";
import { BsDropdownModule } from "ngx-bootstrap/dropdown";

import { WidgetsComponent } from "./widgets.component";
import { WidgetsRoutes } from "./widgets.routing";

@NgModule({
  imports: [
    CommonModule,
    RouterModule.forChild(WidgetsRoutes),
    JwBootstrapSwitchNg2Module,
    BsDropdownModule.forRoot(),
    FormsModule
  ],
  declarations: [WidgetsComponent]
})
export class WidgetsModule {}
