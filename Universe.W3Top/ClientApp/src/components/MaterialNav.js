import React from 'react';
import PropTypes from 'prop-types';
import classNames from 'classnames';
import { withStyles } from '@material-ui/core/styles';
import Drawer from '@material-ui/core/Drawer';
import CssBaseline from '@material-ui/core/CssBaseline';
import AppBar from '@material-ui/core/AppBar';
import Toolbar from '@material-ui/core/Toolbar';
import List from '@material-ui/core/List';
import Typography from '@material-ui/core/Typography';
import Divider from '@material-ui/core/Divider';
import IconButton from '@material-ui/core/IconButton';
import MenuIcon from '@material-ui/icons/Menu';
import ChevronLeftIcon from '@material-ui/icons/ChevronLeft';
import ChevronRightIcon from '@material-ui/icons/ChevronRight';
import ListItem from '@material-ui/core/ListItem';
import ListItemIcon from '@material-ui/core/ListItemIcon';
import ListItemText from '@material-ui/core/ListItemText';
import ListSubheader from '@material-ui/core/ListSubheader';
import InboxIcon from '@material-ui/icons/MoveToInbox';
import MailIcon from '@material-ui/icons/Mail';
import Icon from '@material-ui/core/Icon';
import StarIcon from '@material-ui/icons/Star';
import FlareIcon from '@material-ui/icons/Flare';
import ScoreIcon from '@material-ui/icons/Score';
import InsertChartOutlinedIcon from '@material-ui/icons/InsertChartOutlined';
import InfoOutlinedIcon from '@material-ui/icons/InfoOutlined';
import { Collapse, Container, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';

import { faServer } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { ReactComponent as HardDiskIcon } from '../icons/hard-disk.svg';
import Snackbar from '@material-ui/core/Snackbar';
import CloseIcon from '@material-ui/icons/Close';
import green from '@material-ui/core/colors/green';

import AppGitInfo from "../AppGitInfo"
import dataSourceStore from "../stores/DataSourceStore";
import * as Helper from "../Helper";

import navStore from "../stores/NavStore"
import * as NavActions from "../stores/NavActions"
import * as NewVersionIconDebuggerStore from '../stores/NewVersionIconDebuggerStore'

// ICONS
import { ReactComponent as DisksIconSvg } from '../icons/Disks-Icon.svg';
import { ReactComponent as MainIconSvg } from '../icons/w3top-3.svg';
import { ReactComponent as ProcessListIconSvg } from '../icons/process-list.svg';
const DisksIcon = (size=24,color='#333') => (<DisksIconSvg style={{width: size,height:size,fill:color,strokeWidth:'6px',stroke:color }} />);
// const MainIcon = (size=24,color='#FFF') => (<MainIconSvg style={{width: size,height:size,fill:color,strokeWidth:'1px',stroke:color }} />);
const MainIcon = ({size=40,color='#FFF'}) => (<MainIconSvg style={{width: size,height:size,fill:color,strokeWidth:'1px',stroke:color }} />);
const ProcessListIcon = ({size=24,color='#333'}) => (<ProcessListIconSvg style={{width: size,height:size,fill:color,strokeWidth:'2px',stroke:color }} />);

const drawerWidth = 250;

const styles = theme => ({
    root: {
        display: 'flex',
    },
    appBar: {
        transition: theme.transitions.create(['margin', 'width'], {
            easing: theme.transitions.easing.sharp,
            duration: theme.transitions.duration.leavingScreen,
        }),
    },
    appBarShift: {
        width: `calc(100% - ${drawerWidth}px)`,
        marginLeft: drawerWidth,
        transition: theme.transitions.create(['margin', 'width'], {
            easing: theme.transitions.easing.easeOut,
            duration: theme.transitions.duration.enteringScreen,
        }),
    },
    menuButton: {
        marginLeft: 12,
        marginRight: 20,
    },
    newVersionInfoButton: {
        color: "white",
        fontSize: "10px",
    },
    hide: {
        display: 'none',
    },
    drawer: {
        width: drawerWidth,
        flexShrink: 0,
    },
    drawerPaper: {
        width: drawerWidth,
    },
    drawerHeader: {
        display: 'flex',
        alignItems: 'center',
        padding: '0 8px',
        ...theme.mixins.toolbar,
        justifyContent: 'flex-end',
    },
    drawerVer: {
        display: 'flex',
        alignItems: 'center',
        padding: '0 8px',
        ...theme.mixins.toolbar,
        justifyContent: 'flex-start',
    },
    content: {
        flexGrow: 1,
        padding: theme.spacing.unit * 3,
        paddingBottom: theme.spacing.unit,
        transition: theme.transitions.create('margin', {
            easing: theme.transitions.easing.sharp,
            duration: theme.transitions.duration.leavingScreen,
        }),
        marginLeft: -drawerWidth,
    },
    contentShift: {
        transition: theme.transitions.create('margin', {
            easing: theme.transitions.easing.easeOut,
            duration: theme.transitions.duration.enteringScreen,
        }),
        marginLeft: 0,
    },
    
});

class PersistentDrawerLeft extends React.Component {
    
    state = {
        open: false,
        newVersionOpened: false,
        navKind: 'Welcome'
    };
    
    constructor(props) {
        super(props);
        
        this.updateBrief = this.updateBrief.bind(this);
        this.updateGlobalDataSource = this.updateGlobalDataSource.bind(this);
        this.closeNewVersionBar = this.closeNewVersionBar.bind(this);
    }
    

    componentDidMount() {
        let x1 = dataSourceStore.on('storeUpdated', this.updateGlobalDataSource);
        let x2 = dataSourceStore.on('briefUpdated', this.updateBrief);
        let xNav = navStore.on('storeUpdated', this.handleNav);
        this.handleNav();
    }
    
    handleNav = () => {
        const kind = navStore.getNavKind();
        console.log(`Routing real kind: [${kind}]`);
        this.setState({navKind: kind});
    }
    
    // It is never called, but 
    componentWillUnmount() {
        let x1 = dataSourceStore.removeListener('storeUpdated', this.updateGlobalDataSource);
        let x2 = dataSourceStore.removeListener('briefUpdated', this.updateBrief);
        let xNav = navStore.removeListener('storeUpdated', this.handleNav);
    }


    handleDrawerOpen = () => {
        this.setState({ open: true });
    };

    handleDrawerClose = () => {
        this.setState({ open: false });
    };
    
    updateGlobalDataSource()
    {
        Helper.toConsole("DATASOURCE UPDATED handler AT MaterialNav", dataSourceStore.getDataSource());
        let system = dataSourceStore.getDataSource().system;
        if (Helper.Common.objectIsNotEmpty(system))
            this.setState({system: system});
        
        let newVer = dataSourceStore.getDataSource().newVer;
        let isNewVersionAvailable = PersistentDrawerLeft.getIsNewVersionAvailable(newVer);
        if (isNewVersionAvailable) {
            this.setState({isNewVersionAvailable});
        }
        
    }

    updateBrief()
    {
        console.log("BRIEF UPDATED handler AT MaterialNav");
        this.setState({system: dataSourceStore.getBriefInfo().system});
    }
    
    static getIsNewVersionAvailable(newVer) {
        if (AppGitInfo.CommitCount && newVer && newVer.CommitCount && newVer.CommitCount > AppGitInfo.CommitCount)
            return true;
        
        return false;
    }
    
    sis = {
      def: {
          textAlign: "right",
          paddingRight: 12,
          paddingBottom: 10,
          verticalAlign: "top",
          color: "#888",
          borderBottom: "1px dotted white",
      },
      val: {
          fontWeight: "normal",
          verticalAlign: "top",
          paddingBottom: 10,
      },
      notReady: {
          borderBottom: "1px dotted grey",
          width: 240,
          display: "inline-block", 
          paddingBottom: 0
      },
        ready: {
            borderBottom: "1px dotted white",
            display: "inline-block",
            paddingBottom: 0
        }

    };
    
    closeNewVersionBar() {
        this.setState({newVersionOpened: false});
    }




    render() {
        const newVer = dataSourceStore.getDataSource().newVer;
        const newVerTitle = `New version ${newVer && newVer.Version ? newVer.Version : ""} is available`; 
        const { classes, theme } = this.props;
        const { open } = this.state;
        
        const asHtml = (raw) => <span dangerouslySetInnerHTML={{__html: raw}} />;
        const infoAsComponent = (info) => info ? asHtml(Helper.Common.formatInfoHeader(info)) : null;
        const SysValueNotReady = () => (<span style={this.sis.notReady}>&nbsp;</span>);
        let sysInfoValueId = 0;
        const SysRow = (def,value) => { return (
            <tr>
                <td style={this.sis.def}>{def}</td>
                <td style={this.sis.val}>{ value ? <span style={this.sis.ready} id={`COMMON_INFO_HEADER_` + (++sysInfoValueId)}>{value}</span> : <SysValueNotReady/>}</td>
            </tr>
        )};

        let idMainLink = 0;
        const MainMenuLink = (icon, routeTo, text, subText) => {
            return (
                <ListItem id={`THE_MENU_${++idMainLink}`} button component={NavLink} tag={Link} to={routeTo} key={`${text}@MainMenu`} onClick={() => this.handleDrawerClose()}>
                    <ListItemIcon>{icon}</ListItemIcon>
                    <ListItemText id={`THE_MENU_LINK_${idMainLink}`} primary={text} secondary={subText} />
                </ListItem>
            );
        };

        let system = this.state.system;
        if (!Helper.Common.objectIsNotEmpty(system)) system = {};
        // let hostname = Helper.Common.tryGetProperty(dataSourceStore.getDataSource(), "hostname"); 
        // let [hasSystem, system] = Helper.Common.tryGetProperty(dataSourceStore.getDataSource(), "system");
        // if (!hasSystem) system = {};
        // let hostname = Helper.System.getHostName(dataSourceStore.getDataSource());
        
        let navTitle = null;
        Object.entries(NavActions.NavDefinitions).forEach( ([kind, info]) => {
            if (this.state.navKind === kind)
                navTitle = info.title;
        });
        
        return (
            <div className={classes.root}>
                <CssBaseline />

                <Snackbar 
                    anchorOrigin={{
                        vertical: 'top',
                        horizontal: 'center',
                    }}
                    open={this.state.newVersionOpened}
                    autoHideDuration={6000}
                    onClose={this.closeNewVersionBar}
                    ContentProps={{
                        'aria-describedby': 'message-id',
                    }}
                    message={<span id="message-id">{newVerTitle}</span>}
                    action={[
                        <IconButton
                            key="close"
                            aria-label="Close"
                            color="inherit"
                            className2={"classes.close"}
                            onClick={this.closeNewVersionBar}
                        >
                            <CloseIcon />
                        </IconButton>,
                    ]}
                />
                
                <AppBar
                    position="fixed"
                    className={classNames(classes.appBar, {
                        [classes.appBarShift]: open,
                    })}
                >
                    <Toolbar disableGutters={!open}>
                        <IconButton id={"APP_SYSTEM_ICON"}
                            color="inherit"
                            aria-label="Show menu"
                            onClick={this.handleDrawerOpen}
                            className={classNames(classes.menuButton, open && classes.hide)}
                                    
                        >
                            <MainIcon />
                        </IconButton>
                        <div style={{position:"relative", width: "100%", fontSize: "10px"}}>
                            <div style={{textAlign:"left", width: "100%", position2:"absolute", top:0}}>
                                <Typography variant="overline" color="inherit" noWrap style={{paddingBottom: 0, marginBottom: -10 }}>
                                    W3 Top
                                </Typography>
                                <Typography variant="h6" color="inherit" noWrap>
                                    {navTitle}
                                </Typography>
                            </div>
                            
                            <div style={{textAlign:"right", width: "100%", position:"absolute", top:"3%", fontSize: "10px", display: this.state.isNewVersionAvailable || NewVersionIconDebuggerStore.getForced() ? "block" : "none"}}>
                                <IconButton 
                                    title={newVerTitle}
                                    onClick={() => this.setState({newVersionOpened: true})}
                                    className={classNames(classes.newVersionInfoButton, open && classes.hide)}>
                                    <InfoOutlinedIcon style={{fontSize:"24px"}}/>
                                </IconButton>
                            </div>

                        </div>
                    </Toolbar>
                </AppBar>
                <Drawer
                    className={classes.drawer}
                    variant="persistent"
                    anchor="left"
                    open={open}
                    classes={{
                        paper: classes.drawerPaper,
                    }}
                >
                    <div className={classes.drawerHeader}>
                        <table border="0" cellPadding={0} cellSpacing={0} style={{width: "100%"}}><tbody><tr><td style={{textAlign: "left"}}>

                            <List>
                                    <ListItem button={false}>
                                        <ListItemIcon className={"version"}><InfoOutlinedIcon /></ListItemIcon>
                                        <ListItemText primary={"v" + AppGitInfo.Version} className={"version"} />
                                    </ListItem>
                            </List>
                            
                            {/*<small><InfoOutlinedIcon stye={{width: "9px", fontSize: 44}}/> v{AppGitInfo.Version}</small>*/}
                        </td><td width="24px">
                        <IconButton onClick={this.handleDrawerClose}>
                            {theme.direction === 'ltr' ? <ChevronLeftIcon /> : <ChevronRightIcon />}
                        </IconButton>
                        </td></tr></tbody></table>
                    </div>
                    <Divider />
                    <List>
                        {MainMenuLink(<InsertChartOutlinedIcon/>,"/net", "Network Live Chart")}
                        <Divider />
                        {/*{MainMenuLink(<FontAwesomeIcon icon={faServer} style={{marginLeft:4, marginRight:5}}/>,"Live Mounts", "/mounts")}*/}
                        {/*{MainMenuLink(<HardDiskIcon style={{width:20,height:20,marginLeft:2,marginRight:2,opacity:0.6}}/>,"Live Mounts", "/mounts")}*/}
                        {MainMenuLink(DisksIcon(24,'#333'),"/mounts", "Live Mounts")}
                        {MainMenuLink(<InsertChartOutlinedIcon/>, "/disks", "Disks Live Chart")}
                        {MainMenuLink(<ScoreIcon/>,"/disk-benchmark", "Disk Benchmark")}
                        
                        <Divider />
                        {MainMenuLink(<ProcessListIcon/>,"/processes", "Top Processes" /*,"work in progress"*/)}

                        {(false) &&
                            <>
                            <Divider />
                            <ListSubheader disableSticky={true} style={{
                            paddingTop: 32,
                            paddingBottom: 16,
                            textAlign: "center",
                            lineHeight: "1rem"
                        }}>SAND-BOX<br/>please ignore</ListSubheader>
                            {MainMenuLink(<FlareIcon/>, "/net-v1", "Missed network")}
                            <Divider />
                            {MainMenuLink(<FlareIcon/>, "/1-axis", "Single Y-axis chart")}
                            {MainMenuLink(<FlareIcon/>, "/2-axis", "Double Y-axis one")}
                            </>
                        }
                    </List>
                    <Divider />
                </Drawer>
                <main
                    className={classNames(classes.content, {
                        [classes.contentShift]: open,
                    })}
                >
                    <div className={classes.drawerHeader} />

                    <table border="0" cellSpacing="0" cellPadding="0"><tbody>
                        {SysRow("host", system.hostname)}
                        {SysRow("os", infoAsComponent(system.os))}
                        {SysRow("cpu", infoAsComponent(system.processor))}
                        {SysRow("ram", system.memory)}
                    </tbody></table>
                    
                    <Typography paragraph className={classes.hide}>
                        <FlareIcon /> Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor
                        incididunt ut labore et dolore magna aliqua. Rhoncus dolor purus non enim praesent
                        elementum facilisis leo vel. Risus at ultrices mi tempus imperdiet. Semper risus in
                        hendrerit gravida rutrum quisque non tellus. Convallis convallis tellus id interdum
                        velit laoreet id donec ultrices. Odio morbi quis commodo odio aenean sed adipiscing.
                        Amet nisl suscipit adipiscing bibendum est ultricies integer quis. Cursus euismod quis
                        viverra nibh cras. Metus vulputate eu scelerisque felis imperdiet proin fermentum leo.
                        Mauris commodo quis imperdiet massa tincidunt. Cras tincidunt lobortis feugiat vivamus
                        at augue. At augue eget arcu dictum varius duis at consectetur lorem. Velit sed
                        ullamcorper morbi tincidunt. Lorem donec massa sapien faucibus et molestie ac.
                    </Typography>
                </main>
            </div>
        );
    }
}

PersistentDrawerLeft.propTypes = {
    classes: PropTypes.object.isRequired,
    theme: PropTypes.object.isRequired,
};

export default withStyles(styles, { withTheme: true })(PersistentDrawerLeft);
