import { NgModule } from "@angular/core";
import { RouterModule } from "@angular/router";
import { CommonModule } from "@angular/common";

import { PipeModule } from "../../pipes/pipe.module";
import { DashboardRoutes } from "./dashboard.routing";
import { DashboardComponent } from "./dashboard.component";
import { DashboardService } from "./dashboard.service";

@NgModule({
  imports: [
    CommonModule,
    RouterModule.forChild(DashboardRoutes),
    PipeModule
  ],
  declarations: [
    DashboardComponent
  ],
  providers: [
    DashboardService
  ]
})
export class DashboardModule {}
