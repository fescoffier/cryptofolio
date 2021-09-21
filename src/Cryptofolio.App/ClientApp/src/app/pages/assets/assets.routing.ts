import { Routes } from "@angular/router";

import { AssetDetailComponent } from "./detail/asset-detail.component";

export const AssetsRoutes: Routes = [
  {
    path: ":id",
    component: AssetDetailComponent
  }
];
