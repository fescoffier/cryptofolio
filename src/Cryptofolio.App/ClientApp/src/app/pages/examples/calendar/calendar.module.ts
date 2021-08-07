import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { FormsModule } from "@angular/forms";
import { ModalModule } from "ngx-bootstrap/modal";

import { CalendarComponent } from "./calendar.component";

import { CalendarRoutes } from "./calendar.routing";

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    RouterModule.forChild(CalendarRoutes),
    ModalModule.forRoot()
  ],
  declarations: [CalendarComponent],
  exports: [CalendarComponent]
})
export class CalendarModulee {}
