import { NgModule } from "@angular/core";
import { RouterModule } from "@angular/router";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { BsDropdownModule } from "ngx-bootstrap/dropdown";

import { TimelineComponent } from "./timeline.component";
import { TimelineRoutes } from "./timeline.routing";

@NgModule({
  imports: [
    CommonModule,
    RouterModule.forChild(TimelineRoutes),
    BsDropdownModule.forRoot(),
    FormsModule
  ],
  declarations: [TimelineComponent]
})
export class TimelineModule {}
