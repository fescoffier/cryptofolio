import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { ProgressbarModule } from "ngx-bootstrap/progressbar";


import { PipeModule } from "../../pipes/pipe.module";
import { AssetsRoutes } from "./assets.routing";
import { AssetsService } from "./assets.service";
import { AssetDetailComponent } from "./detail/asset-detail.component";

@NgModule({
  imports: [
    CommonModule,
    RouterModule.forChild(AssetsRoutes),
    PipeModule,
    ProgressbarModule.forRoot()
  ],
  declarations: [
    AssetDetailComponent
  ],
  providers: [
    AssetsService
  ]
})
export class AssetsModule {}
