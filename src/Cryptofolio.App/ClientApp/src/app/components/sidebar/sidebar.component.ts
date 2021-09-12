import { Component, OnInit } from "@angular/core";

export interface RouteInfo {
  path: string;
  title: string;
  type: string;
  icontype: string;
  collapse?: string;
  isCollapsed?: boolean;
  isCollapsing?: any;
  children?: ChildrenItems[];
}

export interface ChildrenItems {
  path: string;
  title: string;
  smallTitle?: string;
  type?: string;
  collapse?: string;
  children?: ChildrenItems2[];
  isCollapsed?: boolean;
}
export interface ChildrenItems2 {
  path?: string;
  smallTitle?: string;
  title?: string;
  type?: string;
}
//Menu Items
export const ROUTES: RouteInfo[] = [
  {
    path: "/dashboard",
    title: "Dashboard",
    type: "link",
    icontype: "tim-icons icon-chart-pie-36"
  },
  {
    path: "/wallets",
    title: "Wallets",
    type: "link",
    icontype: "tim-icons icon-wallet-43"
  },
  {
    path: "/transactions",
    title: "Transactions",
    type: "sub",
    icontype: "tim-icons icon-bullet-list-67",
    collapse: "transactions",
    isCollapsed: false,
    children: [
      {
        path: "history",
        title: "History",
        type: "link",
        smallTitle: "H"
      },
      {
        path: "add",
        title: "Add",
        type: "link",
        smallTitle: "A"
      }
    ]
  }
];

@Component({
  selector: "app-sidebar",
  templateUrl: "./sidebar.component.html",
  styleUrls: ["./sidebar.component.css"]
})
export class SidebarComponent implements OnInit {
  menuItems: any[];

  constructor() {}

  ngOnInit() {
    this.menuItems = ROUTES.filter(menuItem => menuItem);
  }
}
