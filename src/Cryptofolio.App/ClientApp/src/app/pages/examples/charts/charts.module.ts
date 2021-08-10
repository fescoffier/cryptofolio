import { NgModule } from "@angular/core";
import { RouterModule } from "@angular/router";
import { CommonModule } from "@angular/common";

import { ChartsComponent } from "./charts.component";

import { ChartsRoutes } from "./charts.routing";

@NgModule({
  imports: [CommonModule, RouterModule.forChild(ChartsRoutes)],
  declarations: [ChartsComponent],
  exports: [ChartsComponent]
})
export class ChartsModule {}
