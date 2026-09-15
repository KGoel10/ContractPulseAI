
// Vision UI Dashboard React layouts
import Dashboard from "layouts/dashboard";
import SOW from "layouts/sow";
import newRFP from "layouts/newRFP";
import rfp from "layouts/rfp";

// Vision UI Dashboard React icons
import { IoHome } from "react-icons/io5";
import { Drafts, ManageSearch, Restore } from "@mui/icons-material";

const routes = [
  {
    type: "collapse",
    name: "Dashboard",
    key: "dashboard",
    route: "/dashboard",
    icon: <IoHome size="15px" color="inherit" />,
    component: Dashboard,
    noCollapse: true,
  },
   {
    type: "collapse",
    name: "NEW RFP",
    key: "new-rfp",
    route: "/new-rfp",
    icon: <ManageSearch size="15px" color="inherit" />,
    component: newRFP,
    noCollapse: true,
  },
   {
    type: "collapse",
    name: "RFP",
    key: "rfp",
    route: "/rfp/:id?",
    icon: <Drafts size="15px" color="inherit" />,
    component: rfp,
    noCollapse: true,
  },
  {
    type: "collapse",
    name: "SOW",
    key: "sow",
    route: "/sow",
    icon: <Restore size="15px" color="inherit" />,
    component: SOW,
    noCollapse: true,
  },
  //{ type: "title", title: "Account Pages", key: "account-pages" },
  // {
  //   type: "collapse",
  //   name: "Tables",
  //   key: "tables",
  //   route: "/tables",
  //   icon: <IoStatsChart size="15px" color="inherit" />,
  //   component: Tables,
  //   noCollapse: true,
  // },
  // {
  //   type: "collapse",
  //   name: "RTL",
  //   key: "rtl",
  //   route: "/rtl",
  //   icon: <IoBuild size="15px" color="inherit" />,
  //   component: RTL,
  //   noCollapse: true,
  // },
 
];

export default routes;
