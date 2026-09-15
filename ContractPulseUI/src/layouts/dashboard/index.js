import { useEffect, useState } from "react";
import { useHistory } from "react-router-dom";
import {
  Alert,
  Card,
  CircularProgress,
  Grid,
  IconButton,
  Link,
  Tooltip,
} from "@mui/material";
import EditIcon from "@mui/icons-material/Edit";
import OpenInNewIcon from "@mui/icons-material/OpenInNew";
import api from "services/axios";

import VuiBox from "components/VuiBox";
import VuiTypography from "components/VuiTypography";
import DashboardLayout from "examples/LayoutContainers/DashboardLayout";
import DashboardNavbar from "examples/Navbars/DashboardNavbar";
import Footer from "examples/Footer";
import Table from "examples/Tables/Table";

const USE_DEMO_DATA = true;

const demoRfps = [
  {
    id: 101,
    clientName: "Northstar Health",
    clientEmail: "procurement@northstar.example",
    rfpStatus: "SOW_Generated",
    sowLink: "https://example.com/northstar-sow",
  },
  {
    id: 102,
    clientName: "Atlas Retail Group",
    clientEmail: "projects@atlas.example",
    rfpStatus: "RFP_Generated",
    sowLink: "",
  },
  {
    id: 103,
    clientName: "Pioneer Logistics",
    clientEmail: "technology@pioneer.example",
    rfpStatus: "In Progress",
    sowLink: "",
  },
];

const tableColumns = [
  { name: "id", align: "left", width: "8%" },
  { name: "client", align: "left", width: "20%" },
  { name: "email", align: "left", width: "24%" },
  { name: "status", align: "left", width: "17%" },
  { name: "sow", align: "left", width: "18%" },
  { name: "actions", align: "right", width: "13%" },
];

function Dashboard() {
  const history = useHistory();
  const [rfps, setRfps] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    const loadRfps = async () => {
      try {
        const result = await api.get("/api/Rfp");
        setRfps(Array.isArray(result) ? result : []);
      } catch (requestError) {
        setError(
          requestError.response?.data?.message ||
            requestError.message ||
            "Unable to load RFPs."
        );
      } finally {
        setIsLoading(false);
      }
    };

    loadRfps();
  }, []);

  const displayedRfps = USE_DEMO_DATA ? demoRfps : rfps;
  const tableRows = displayedRfps.map((rfp) => ({
    id: rfp.id,
    client: rfp.clientName,
    email: rfp.clientEmail,
    status: (
      <VuiTypography
        variant="button"
        color={rfp.rfpStatus === "SOW_Generated" ? "success" : "text"}
        fontWeight="medium"
      >
        {rfp.rfpStatus || "Pending"}
      </VuiTypography>
    ),
    sow: rfp.sowLink ? (
      <Link
        href={rfp.sowLink}
        target="_blank"
        rel="noreferrer"
        sx={{ color: "#66b3ff", fontSize: "0.75rem", fontWeight: 500 }}
      >
        Open SOW
      </Link>
    ) : (
      <VuiTypography variant="button" color="text">
        Not generated
      </VuiTypography>
    ),
    actions: (
      <VuiBox display="flex" justifyContent="flex-end" alignItems="center">
        <Tooltip title="Edit RFP">
          <IconButton
            aria-label={`Edit RFP ${rfp.id}`}
            onClick={() => history.push(`/rfp/${rfp.id}`)}
            sx={{ color: "#66b3ff" }}
          >
            <EditIcon fontSize="small" />
          </IconButton>
        </Tooltip>
        <Tooltip title="Open SOW">
          <IconButton
            aria-label={`Open SOW ${rfp.id}`}
            onClick={() => history.push(`/sow/${rfp.id}`)}
            sx={{ color: "#66b3ff" }}
          >
            <OpenInNewIcon fontSize="small" />
          </IconButton>
        </Tooltip>
      </VuiBox>
    ),
  }));

  return (
    <DashboardLayout>
      <DashboardNavbar />
      <VuiBox py={3}>
        <Grid container spacing={3}>
          <Grid item xs={12}>
            <Card sx={{ height: "100% !important", overflow: "hidden" }}>
              <VuiBox p={{ xs: 2, md: 3 }}>
                <VuiBox display="flex" flexDirection="column" mb={3}>
                  <VuiTypography color="white" variant="lg" display="block" mb="6px">
                    RFPs Overview
                  </VuiTypography>
                  <VuiTypography variant="button" display="block" fontWeight="regular" color="text">
                    Track client proposals and generated statements of work.
                  </VuiTypography>
                </VuiBox>
                {!USE_DEMO_DATA && error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
                {!USE_DEMO_DATA && isLoading ? (
                  <VuiBox display="flex" justifyContent="center" py={4}>
                    <CircularProgress color="info" size={28} />
                  </VuiBox>
                ) : displayedRfps.length === 0 ? (
                  <VuiTypography variant="button" color="text" py={3}>
                    No RFPs found.
                  </VuiTypography>
                ) : (
                  <Table columns={tableColumns} rows={tableRows} />
                )}
              </VuiBox>
            </Card>
          </Grid>
        </Grid>
      </VuiBox>
      {/* <Footer /> */}
    </DashboardLayout>
  );
}

export default Dashboard;
