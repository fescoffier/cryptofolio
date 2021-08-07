import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from "@angular/core";
import { RouterModule } from "@angular/router";
import { CommonModule } from "@angular/common";
import { DxVectorMapModule } from "devextreme-angular";

import { GoogleComponent } from "./google/google.component";
import { FullscreenComponent } from "./fullscreen/fullscreen.component";
import { VectorComponent } from "./vector/vector.component";

import { MapsRoutes } from "./maps.routing";

@NgModule({
  imports: [CommonModule, RouterModule.forChild(MapsRoutes), DxVectorMapModule],
  declarations: [GoogleComponent, FullscreenComponent, VectorComponent],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class MapsModule {}
