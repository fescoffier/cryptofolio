import { NgModule } from "@angular/core";
import { RouterModule } from "@angular/router";
import { CommonModule } from "@angular/common";
import { NgxDatatableModule } from "@swimlane/ngx-datatable";
import { ProgressbarModule } from "ngx-bootstrap/progressbar";
import { TooltipModule } from "ngx-bootstrap/tooltip";
import { BsDropdownModule } from "ngx-bootstrap/dropdown";

import { RegularComponent } from "./regular/regular.component";
import { ExtendedComponent } from "./extended/extended.component";
import { TablesRoutes } from "./tables.routing";
import { NgxDatatablesComponent } from "./ngxdatatables/ngxdatatables.component";

@NgModule({
  imports: [
    CommonModule,
    RouterModule.forChild(TablesRoutes),
    NgxDatatableModule,
    TooltipModule.forRoot(),
    BsDropdownModule.forRoot(),
    ProgressbarModule.forRoot()
  ],
  declarations: [RegularComponent, ExtendedComponent, NgxDatatablesComponent]
})
export class TablesModule {}
