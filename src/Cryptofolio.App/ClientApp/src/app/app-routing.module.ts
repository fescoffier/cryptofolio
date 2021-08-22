import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { BrowserModule } from "@angular/platform-browser";
import { Routes, RouterModule } from "@angular/router";

import { AdminLayoutComponent } from "./layouts/admin-layout/admin-layout.component";
import { AuthLayoutComponent } from "./layouts/auth-layout/auth-layout.component";

const routes: Routes = [
  {
    path: "",
    redirectTo: "dashboard",
    pathMatch: "full"
  },
  {
    path: "",
    component: AdminLayoutComponent,
    children: [
      {
        path: "",
        loadChildren:
          "./pages/examples/dashboard/dashboard.module#DashboardModule"
      },
      {
        path: "wallets",
        loadChildren:
          "./pages/wallet/wallet.module#WalletModule"
      },
      {
        path: "transactions",
        loadChildren:
          "./pages/transaction/transaction.module#TransactionModule"
      },
      {
        path: "components",
        loadChildren:
          "./pages/examples/components/components.module#ComponentsPageModule"
      },
      {
        path: "forms",
        loadChildren: "./pages/examples/forms/forms.module#Forms"
      },
      {
        path: "tables",
        loadChildren: "./pages/examples/tables/tables.module#TablesModule"
      },
      {
        path: "maps",
        loadChildren: "./pages/examples/maps/maps.module#MapsModule"
      },
      {
        path: "widgets",
        loadChildren: "./pages/examples/widgets/widgets.module#WidgetsModule"
      },
      {
        path: "charts",
        loadChildren: "./pages/examples/charts/charts.module#ChartsModule"
      },
      {
        path: "calendar",
        loadChildren:
          "./pages/examples/calendar/calendar.module#CalendarModulee"
      },
      {
        path: "",
        loadChildren:
          "./pages/examples/pages/user/user-profile.module#UserModule"
      },
      {
        path: "",
        loadChildren:
          "./pages/examples/pages/timeline/timeline.module#TimelineModule"
      }
    ]
  },
  {
    path: "",
    component: AuthLayoutComponent,
    children: [
      {
        path: "pages",
        loadChildren: "./pages/examples/pages/pages.module#PagesModule"
      }
    ]
  },
  {
    path: "**",
    redirectTo: "dashboard"
  }
];

@NgModule({
  imports: [
    CommonModule,
    BrowserModule,
    RouterModule.forRoot(routes, {
      useHash: true,
      scrollPositionRestoration: "enabled",
      anchorScrolling: "enabled",
      scrollOffset: [0, 64]
    })
  ],
  exports: [RouterModule]
})
export class AppRoutingModule {}
